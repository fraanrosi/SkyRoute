export interface Airport {
  code: string;
  name: string;
  city: string;
  country: string;
}

export enum CabinClass {
  Economy = 0,
  Business = 1,
  First = 2
}

export enum DocumentType {
  Passport = 0,
  NationalId = 1
}

export enum BookingStatus {
  Confirmed = 0,
  Pending = 1,
  Failed = 2
}

export function isInternational(originCountry: string | undefined, destinationCountry: string | undefined): boolean {
  if (!originCountry || !destinationCountry) return false;
  return originCountry.trim().toLowerCase() !== destinationCountry.trim().toLowerCase();
}
