import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateProjectInput, Project } from '../models/project.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectApiService {
  private apiUrl = 'https://localhost:44313/api/projects';

  constructor(private http: HttpClient) {}

  get(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl);
  }

  create(input: CreateProjectInput): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
