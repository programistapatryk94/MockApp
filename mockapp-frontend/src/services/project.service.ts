import { Injectable } from '@angular/core';
import { Project } from './models/project.model';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private currentProject$ = new BehaviorSubject<Project | null>(null);

  constructor() {}

  public setCurrentProject(project: Project): void {
    this.currentProject$.next(project);
  }

  getCurrentProject(): Observable<Project | null> {
    return this.currentProject$.asObservable();
  }

  clearCurrentProject(): void {
    this.currentProject$.next(null);
  }
}
