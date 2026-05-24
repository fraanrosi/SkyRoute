import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { BookingListItem } from '../models/booking.models';
import { BookingService } from '../services/booking.service';

@Component({
  selector: 'app-bookings-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './bookings-page.component.html',
  styleUrl: './bookings-page.component.scss'
})
export class BookingsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly bookingService = inject(BookingService);

  readonly lookupForm = this.fb.nonNullable.group({
    reference: ['', [Validators.required, Validators.pattern(/^SKY-\d{4}-[A-F0-9]{6}$/i)]]
  });

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly bookings = signal<BookingListItem[]>([]);

  readonly statusLabels = ['Confirmed', 'Pending', 'Failed'];

  ngOnInit(): void {
    this.bookingService.getAllBookings().subscribe({
      next: list => {
        this.bookings.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load bookings. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  lookup(): void {
    if (this.lookupForm.invalid) {
      this.lookupForm.markAllAsTouched();
      return;
    }
    const reference = this.lookupForm.controls.reference.value.trim().toUpperCase();
    this.router.navigate(['/booking/confirmation', reference]);
  }
}
