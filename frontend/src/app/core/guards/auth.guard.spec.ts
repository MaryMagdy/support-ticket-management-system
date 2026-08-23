import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;

  const executeGuard = () =>
    TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [{ provide: AuthService, useValue: authServiceSpy }],
    });

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
  });

  it('allows navigation when the user is logged in', () => {
    authService.isLoggedIn.and.returnValue(true);

    const result = executeGuard();

    expect(result).toBeTrue();
  });

  it('redirects to /login when the user is not logged in', () => {
    authService.isLoggedIn.and.returnValue(false);

    const result = executeGuard() as UrlTree;

    expect(result).toBeInstanceOf(UrlTree);
    expect(result.toString()).toBe('/login');
  });
});
