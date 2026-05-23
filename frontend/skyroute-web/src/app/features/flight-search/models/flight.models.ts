import { CabinClass } from '../../../shared/models/shared.models';

export interface FlightSearchRequest {
  originAirportCode: string;
  destinationAirportCode: string;
  departureDate: string;
  numberOfPassengers: number;
  cabinClass: CabinClass;
}

export interface FlightResult {
  flightId: string;
  airlineProvider: string;
  flightNumber: string;
  originAirportCode: string;
  destinationAirportCode: string;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  cabinClass: CabinClass;
  pricePerPassenger: number;
  totalPrice: number;
  numberOfPassengers: number;
}

export type SortOption = 'priceAsc' | 'priceDesc' | 'durationAsc' | 'departureAsc';
