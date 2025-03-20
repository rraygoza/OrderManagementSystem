import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: false,
})
export class AppComponent implements OnInit {
  title = 'OrderManagementApp';
  isAuth = false;
   constructor(public authService: AuthService, private router: Router) {}
  ngOnInit(): void {
    this.isAuth = this.authService.isAuthenticated();
  }
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}