import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { BookingResponse } from '../models/booking.models';
import { BookingService } from '../services/booking.service';

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './booking-confirmation.component.html',
  styleUrl: './booking-confirmation.component.scss'
})
export class BookingConfirmationComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly bookingService = inject(BookingService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly booking = signal<BookingResponse | null>(null);

  readonly statusLabels = ['Confirmed', 'Pending', 'Failed'];

  ngOnInit(): void {
    const reference = this.route.snapshot.paramMap.get('reference');
    if (!reference) {
      this.error.set('Missing booking reference.');
      this.loading.set(false);
      return;
    }

    this.bookingService.getBooking(reference).subscribe({
      next: response => {
        this.booking.set(response);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(`We could not find booking ${reference}.`);
        this.loading.set(false);
      }
    });
  }
}
