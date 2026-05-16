/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authServiceMock: any;
  let cartServiceMock: any;
  let router: Router;

  let userSubject: BehaviorSubject<any>;
  let cartSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    // ---- Mock streams ----
    userSubject = new BehaviorSubject<any>(null);
    cartSubject = new BehaviorSubject<any>([]);

    authServiceMock = {
      currentUser$: userSubject.asObservable(),
      logout: jasmine.createSpy('logout')
    };

    cartServiceMock = {
      cart$: cartSubject.asObservable(),
      getCartCount: jasmine.createSpy('getCartCount').and.returnValue(0)
    };

    await TestBed.configureTestingModule({
      imports: [
        NavbarComponent,
        RouterTestingModule 
      ],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: CartService, useValue: cartServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);

    spyOn(router, 'navigate');

    fixture.detectChanges();
  });

  // ---------------- TESTS ----------------

  it('should initialize with logged out state', () => {
    expect(component.isLoggedIn).toBeFalse();
    expect(component.isAdmin).toBeFalse();
  });

  it('should update state when user logs in (admin)', () => {
    userSubject.next({
      Name: 'John Admin',
      Role: 'Admin'
    });

    expect(component.isLoggedIn).toBeTrue();
    expect(component.isAdmin).toBeTrue();
    expect(component.userName).toBe('John');
  });

  it('should update state when user logs in (customer)', () => {
    userSubject.next({
      Name: 'Bob User',
      Role: 'Customer'
    });

    expect(component.isLoggedIn).toBeTrue();
    expect(component.isAdmin).toBeFalse();
    expect(component.userName).toBe('Bob');
  });

  it('should update cart count on cart change', () => {
    cartServiceMock.getCartCount.and.returnValue(3);

    cartSubject.next([{ id: 1 }, { id: 2 }, { id: 3 }]);

    expect(component.cartCount).toBe(3);
  });

  it('should logout and navigate to home', () => {
    component.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should navigate to search results when query is valid', () => {
    component.searchQuery = 'laptop';

    component.search();

    expect(router.navigate).toHaveBeenCalledWith(['/products'], {
      queryParams: { q: 'laptop' }
    });
  });

  it('should NOT navigate when search query is empty', () => {
    component.searchQuery = '   ';

    component.search();

    expect(router.navigate).not.toHaveBeenCalled();
  });

});