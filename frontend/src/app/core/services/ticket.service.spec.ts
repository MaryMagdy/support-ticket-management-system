import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TicketService } from './ticket.service';
import { environment } from '../../../environments/environment';
import { TicketPriority, TicketStatus } from '../models';

describe('TicketService', () => {
  let service: TicketService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TicketService],
    });
    service = TestBed.inject(TicketService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getTickets calls the correct URL with query params', () => {
    const mockResponse = { items: [], totalCount: 0, page: 1, pageSize: 10 };

    service
      .getTickets({
        page: 1,
        pageSize: 10,
        status: TicketStatus.Open,
        priority: TicketPriority.High,
        search: 'printer',
        sortBy: 'CreatedAt',
        descending: true,
      })
      .subscribe((res) => {
        expect(res).toEqual(mockResponse);
      });

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/tickets`
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('status')).toBe(TicketStatus.Open);
    expect(req.request.params.get('priority')).toBe(TicketPriority.High);
    expect(req.request.params.get('search')).toBe('printer');
    expect(req.request.params.get('sortBy')).toBe('CreatedAt');
    expect(req.request.params.get('descending')).toBe('true');
    req.flush(mockResponse);
  });

  it('getTicket calls the correct URL', () => {
    service.getTicket(42).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/tickets/42`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('createTicket posts to the tickets endpoint', () => {
    const payload = { title: 't', description: 'd', priority: TicketPriority.Low };
    service.createTicket(payload).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/tickets`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });
});
