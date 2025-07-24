import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from '../../app/shared/app-config.service';
import { Observable } from 'rxjs';
import { ICreateCheckoutSessionInput, PaymentInfo } from '../models/subscription.model';

@Injectable()
export class SubscriptionApiService {
  private readonly apiUrl;

  constructor(private http: HttpClient, private config: AppConfigService) {
    this.apiUrl = `${this.config.remoteApiUrl}/api/stripe`;
  }

  startSubscription(
    input: ICreateCheckoutSessionInput
  ): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(
      `${this.apiUrl}/create-checkout-session`,
      input
    );
  }

  cancelSubscription(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/cancel-at-period-end`, {});
  }

  getPaymentInfo(sessionId: string): Observable<PaymentInfo> {
    const params = new HttpParams().set('sessionId', sessionId);
    return this.http.get<PaymentInfo>(`${this.apiUrl}/payment-info`, {params});
  }
}
