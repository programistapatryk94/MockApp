import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SubscriptionApiService } from '../../../services/apis/subscription-api.service';
import { filter, finalize, switchMap, tap, timer } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { PaymentInfo } from '../../../services/models/subscription.model';
import { CommonModule } from '@angular/common';
import { BusyDirective } from '../../shared/directives/busy.directive';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-success',
  templateUrl: './success.component.html',
  styleUrls: ['./success.component.scss'],
  standalone: true,
  imports: [MatCardModule, CommonModule, BusyDirective, TranslateModule],
})
export class SuccessComponent implements OnInit {
  loading: boolean = false;
  paymentInfo: PaymentInfo | null = null;

  constructor(
    private route: ActivatedRoute,
    private subscriptionApiService: SubscriptionApiService,
    private router: Router
  ) {}

  ngOnInit() {
    const sessionId = this.route.snapshot.queryParamMap.get('session_id');

    if (sessionId != null) {
      this.loading = true;
      this.subscriptionApiService
        .getPaymentInfo(sessionId)
        .pipe(
          tap((res) => {
            this.paymentInfo = res;
          }),
          finalize(() => (this.loading = false))
        )
        .subscribe((res) => {
          if (res.paymentStatus === 'paid') {
            timer(5000).subscribe(() => this.router.navigate(['/projects']));
          }
        });
    }
  }
}
