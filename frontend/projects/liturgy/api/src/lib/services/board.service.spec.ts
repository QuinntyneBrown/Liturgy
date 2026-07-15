import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from './api-config';
import { BoardService } from './board.service';

describe('BoardService', () => {
  let service: BoardService;
  let http: HttpTestingController;
  const baseUrl = 'http://test.local';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    service = TestBed.inject(BoardService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('moves a card by posting the target column', () => {
    service.moveCard('card-1', 'Done').subscribe();

    const req = http.expectOne(`${baseUrl}/api/board/cards/card-1/move`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ column: 'Done' });
    req.flush({});
  });

  it('gets the board for a project', () => {
    service.get('project-1').subscribe();

    const req = http.expectOne(`${baseUrl}/api/board/project-1`);
    expect(req.request.method).toBe('GET');
    req.flush({ projectId: 'project-1', sprintId: 's', sprintNumber: 6, cards: [] });
  });
});
