import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from '../../app/shared/app-config.service';
import { Observable } from 'rxjs';

@Injectable()
export class SubscriptionApiService {
  private readonly apiUrl;

  constructor(private http: HttpClient, private config: AppConfigService) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/stripe`;
  }

  startSubscription(): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(
      `${this.apiUrl}/create-checkout-session`,
      {}
    );
  }
}
