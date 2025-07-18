import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from '../../app/shared/app-config.service';
import { Observable } from 'rxjs';
import { ISubscriptionPlanDto } from '../models/subscription-plan.model';

@Injectable()
export class SubscriptionPlanApiService {
  private readonly apiUrl;

  constructor(private http: HttpClient, private config: AppConfigService) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/subscriptionPlans`;
  }

  getAll(): Observable<ISubscriptionPlanDto[]> {
    return this.http.get<ISubscriptionPlanDto[]>(`${this.apiUrl}`);
  }
}
