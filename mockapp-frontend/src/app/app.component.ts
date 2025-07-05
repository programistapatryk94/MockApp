import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../helpers/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatToolbarModule, MatButtonModule, CommonModule, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'mockapp-frontend';

  constructor(private auth: AuthService, private router: Router) {}

  isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
