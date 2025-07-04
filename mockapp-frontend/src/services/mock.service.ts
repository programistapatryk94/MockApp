import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateMockInput, Mock } from './models/mock.model';

@Injectable({
  providedIn: 'root',
})
export class MockService {
  private apiUrl = 'https://localhost:44313/api/mock';

  constructor(private http: HttpClient) {}

  getMocks(): Observable<Mock[]> {
    return this.http.get<Mock[]>(this.apiUrl);
  }

  createMock(mock: CreateMockInput): Observable<Mock> {
    return this.http.post<Mock>(this.apiUrl, mock);
  }
}
