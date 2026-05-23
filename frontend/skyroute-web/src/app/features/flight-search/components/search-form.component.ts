import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Airport, CabinClass } from '../../../shared/models/shared.models';
import { AirportService } from '../../../shared/services/airport.service';
import { FlightSearchRequest } from '../models/flight.models';

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './search-form.component.html',
  styleUrl: './search-form.component.scss'
})
export class SearchFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly airportService = inject(AirportService);

  @Input() loading = false;
  @Output() readonly searchRequested = new EventEmitter<FlightSearchRequest>();

  airports: Airport[] = [];
  airportsError: string | null = null;

  readonly cabinClasses = [
    { value: CabinClass.Economy,  label: 'Economy'  },
    { value: CabinClass.Business, label: 'Business' },
    { value: CabinClass.First,    label: 'First'    }
  ];

  readonly form = this.fb.nonNullable.group({
    originAirportCode: ['', Validators.required],
    destinationAirportCode: ['', Validators.required],
    departureDate: [this.todayIso(), Validators.required],
    numberOfPassengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
    cabinClass: [CabinClass.Economy, Validators.required]
  });

  ngOnInit(): void {
    this.airportService.getAirports().subscribe({
      next: list => { this.airports = list; },
      error: () => { this.airportsError = 'Could not load airports. Is the API running?'; }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { originAirportCode, destinationAirportCode } = this.form.getRawValue();
    if (originAirportCode === destinationAirportCode) {
      this.form.controls.destinationAirportCode.setErrors({ sameAirport: true });
      return;
    }

    this.searchRequested.emit(this.form.getRawValue());
  }

  private todayIso(): string {
    const today = new Date();
    today.setDate(today.getDate() + 1);
    return today.toISOString().slice(0, 10);
  }
}
