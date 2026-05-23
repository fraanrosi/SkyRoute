import { BookingStatus, DocumentType } from '../../../shared/models/shared.models';

export interface PassengerDto {
  fullName: string;
  email: string;
  documentNumber: string;
  documentType: DocumentType;
}

export interface BookingRequest {
  flightId: string;
  passengers: PassengerDto[];
  contactEmail: string;
}

export interface BookingResponse {
  bookingReference: string;
  bookingDate: string;
  flightId: string;
  totalPriceCharged: number;
  status: BookingStatus;
}
