import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { AirportService } from '../../../shared/services/airport.service';
import { DocumentType } from '../../../shared/models/shared.models';
import { FlightResult } from '../../flight-search/models/flight.models';
import { FlightSearchService } from '../../flight-search/services/flight-search.service';
import { BookingRequest } from '../models/booking.models';
import { BookingService } from '../services/booking.service';
import { PassengerFormComponent } from './passenger-form.component';
import { passportValidator } from '../../../shared/validators/passport.validator';
import { nationalIdValidator } from '../../../shared/validators/national-id.validator';

@Component({
  selector: 'app-booking-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PassengerFormComponent],
  templateUrl: './booking-page.component.html',
  styleUrl: './booking-page.component.scss'
})
export class BookingPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly flightSearch = inject(FlightSearchService);
  private readonly airportService = inject(AirportService);
  private readonly bookingService = inject(BookingService);

  readonly flight = signal<FlightResult | null>(null);
  readonly isInternational = signal(false);
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly documentLabel = computed(() =>
    this.isInternational() ? 'Passport Number' : 'National ID'
  );

  readonly documentHint = computed(() =>
    this.isInternational()
      ? '6–9 alphanumeric uppercase characters (e.g. AB123456).'
      : '7–8 digits (e.g. 12345678).'
  );

  readonly form: FormGroup = this.fb.nonNullable.group({
    contactEmail: ['', [Validators.required, Validators.email]],
    passengers: this.fb.array<FormGroup>([])
  });

  get passengers(): FormArray<FormGroup> {
    return this.form.get('passengers') as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    const flightId = this.route.snapshot.paramMap.get('flightId') ?? '';
    const flight = this.flightSearch.findById(flightId);

    if (!flight) {
      this.submitError.set('Selected flight is no longer available. Please search again.');
      return;
    }

    this.flight.set(flight);
    this.airportService.getAirports().subscribe(() => {
      this.isInternational.set(
        this.airportService.isInternationalRoute(flight.originAirportCode, flight.destinationAirportCode)
      );
      this.rebuildPassengers(flight.numberOfPassengers);
    });
  }

  private rebuildPassengers(count: number): void {
    this.passengers.clear();
    for (let i = 0; i < count; i++) {
      this.passengers.push(this.buildPassengerGroup());
    }
  }

  private buildPassengerGroup(): FormGroup {
    return this.fb.nonNullable.group({
      fullName:       ['', [Validators.required, Validators.minLength(2)]],
      email:          ['', [Validators.required, Validators.email]],
      documentNumber: ['', [Validators.required, this.documentValidator()]]
    });
  }

  private documentValidator() {
    return this.isInternational() ? passportValidator : nationalIdValidator;
  }

  submit(): void {
    if (!this.flight() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const flight = this.flight()!;
    const docType = this.isInternational() ? DocumentType.Passport : DocumentType.NationalId;
    const raw = this.form.getRawValue() as {
      contactEmail: string;
      passengers: Array<{ fullName: string; email: string; documentNumber: string }>;
    };

    const request: BookingRequest = {
      flightId: flight.flightId,
      contactEmail: raw.contactEmail,
      passengers: raw.passengers.map(p => ({
        fullName: p.fullName,
        email: p.email,
        documentNumber: this.isInternational() ? p.documentNumber.toUpperCase() : p.documentNumber,
        documentType: docType
      }))
    };

    this.submitting.set(true);
    this.submitError.set(null);
    this.bookingService.createBooking(request).subscribe({
      next: response => {
        this.router.navigate(['/booking/confirmation', response.bookingReference]);
      },
      error: err => {
        const message = err?.error?.error ?? 'Booking failed. Please review your details and try again.';
        this.submitError.set(message);
        this.submitting.set(false);
      }
    });
  }
}
