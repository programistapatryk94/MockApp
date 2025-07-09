import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateProjectInput, Project } from '../models/project.model';
import { AppConfigService } from '../../app/shared/app-config.service';

@Injectable()
export class ProjectApiService {
  private readonly apiUrl;

  constructor(
    private http: HttpClient,
    private config: AppConfigService
  ) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/projects`;
  }

  get(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl);
  }

  create(input: CreateProjectInput): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, input);
  }

  update(projectId: string, project: CreateProjectInput): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${projectId}`, project);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
