import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BookingRequest, BookingResponse } from '../models/booking.models';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly http = inject(HttpClient);

  createBooking(request: BookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>('/api/bookings', request);
  }

  getBooking(reference: string): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`/api/bookings/${encodeURIComponent(reference)}`);
  }
}
