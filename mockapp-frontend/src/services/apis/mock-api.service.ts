import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateMockInput, Mock } from '../models/mock.model';
import { AppConfigService } from '../../app/shared/app-config.service';

@Injectable()
export class MockApiService {
  private readonly apiUrl;

  constructor(
    private http: HttpClient,
    private config: AppConfigService
  ) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/mocks`;
  }

  getAll(projectId: string): Observable<Mock[]> {
    const params = new HttpParams().set('projectId', projectId);

    return this.http.get<Mock[]>(this.apiUrl, { params });
  }

  create(mock: CreateMockInput): Observable<Mock> {
    return this.http.post<Mock>(this.apiUrl, mock);
  }

  update(mockId: string, mock: CreateMockInput): Observable<Mock> {
    return this.http.put<Mock>(`${this.apiUrl}/${mockId}`, mock);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
