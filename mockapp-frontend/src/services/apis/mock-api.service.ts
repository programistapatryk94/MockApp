import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateMockInput, Mock } from '../models/mock.model';

@Injectable({
  providedIn: 'root',
})
export class MockApiService {
  private apiUrl = 'https://localhost:44313/api/mocks';

  constructor(private http: HttpClient) {}

  getMocks(projectId: string): Observable<Mock[]> {

    const params = new HttpParams().set('projectId', projectId);

    return this.http.get<Mock[]>(this.apiUrl, {params});
  }

  createMock(mock: CreateMockInput): Observable<Mock> {
    return this.http.post<Mock>(this.apiUrl, mock);
  }
}
