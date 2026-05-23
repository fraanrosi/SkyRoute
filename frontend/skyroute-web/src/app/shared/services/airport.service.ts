import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay, tap } from 'rxjs';

import { Airport, isInternational } from '../models/shared.models';

@Injectable({ providedIn: 'root' })
export class AirportService {
  private readonly http = inject(HttpClient);

  private cache$: Observable<Airport[]> | null = null;
  private byCode: Map<string, Airport> = new Map();

  getAirports(): Observable<Airport[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<Airport[]>('/api/airports').pipe(
        tap(list => {
          this.byCode.clear();
          for (const a of list) this.byCode.set(a.code.toUpperCase(), a);
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.cache$;
  }

  findByCodeSync(code: string | null | undefined): Airport | undefined {
    if (!code) return undefined;
    return this.byCode.get(code.toUpperCase());
  }

  isInternationalRoute(originCode: string | null | undefined, destinationCode: string | null | undefined): boolean {
    const origin = this.findByCodeSync(originCode);
    const destination = this.findByCodeSync(destinationCode);
    return isInternational(origin?.country, destination?.country);
  }
}
