import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { FlightResult, FlightSearchRequest } from '../models/flight.models';

@Injectable({ providedIn: 'root' })
export class FlightSearchService {
  private readonly http = inject(HttpClient);

  readonly lastResults = signal<FlightResult[]>([]);
  readonly lastRequest = signal<FlightSearchRequest | null>(null);

  search(request: FlightSearchRequest): Observable<FlightResult[]> {
    this.lastRequest.set(request);
    return this.http.post<FlightResult[]>('/api/flights/search', request).pipe(
      tap(results => this.lastResults.set(results))
    );
  }

  findById(flightId: string): FlightResult | undefined {
    return this.lastResults().find(f => f.flightId === flightId);
  }
}
