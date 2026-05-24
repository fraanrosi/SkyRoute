import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app.component';
import { routes } from './app.routes';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter(routes)]
    }).compileComponents();
  });

  it('creates the app shell', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders the SkyRoute brand and navigation links', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelector('.brand')?.textContent).toContain('SkyRoute');

    const navLinks = Array.from(host.querySelectorAll('nav a')).map(a => a.textContent?.trim());
    expect(navLinks).toContain('Search flights');
    expect(navLinks).toContain('Bookings');
  });
});
