import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: false,
})
export class LoginComponent {
  credentials = { username: '', password: '' };
  loginError = false;

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(): void {
      this.authService.login(this.credentials).subscribe(
          (response: any) => {
              localStorage.setItem('authToken', response.token);
              this.router.navigate(['/']);

          },
          (error: any) => {
              console.error("Login error:", error);
              this.loginError = true;
          }
      );
  }
}