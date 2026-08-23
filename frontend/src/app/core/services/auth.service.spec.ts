import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { UserRole } from '../models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('login success sets token and current user state', () => {
    const mockResponse = {
      accessToken: 'abc123',
      refreshToken: 'refresh123',
      expiresAt: '2030-01-01T00:00:00Z',
      user: { id: 1, fullName: 'Jane', email: 'jane@test.com', role: UserRole.Customer },
    };

    service.login({ email: 'jane@test.com', password: 'password' }).subscribe((res) => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.token).toBe('abc123');
    expect(service.currentUser?.email).toBe('jane@test.com');
  });

  it('logout clears token and current user state', () => {
    const mockResponse = {
      accessToken: 'abc123',
      refreshToken: 'refresh123',
      expiresAt: '2030-01-01T00:00:00Z',
      user: { id: 1, fullName: 'Jane', email: 'jane@test.com', role: UserRole.Customer },
    };
    service.setSession(mockResponse);
    expect(service.isLoggedIn()).toBeTrue();

    service.logout();

    expect(service.isLoggedIn()).toBeFalse();
    expect(service.token).toBeNull();
    expect(service.currentUser).toBeNull();
  });
});
