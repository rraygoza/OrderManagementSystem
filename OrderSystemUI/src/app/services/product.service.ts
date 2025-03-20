import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private serviceUrl = "http://localhost:5001/api";
  constructor(private http: HttpClient,  private authService: AuthService) { }

   private getHeaders(): HttpHeaders {
        const token = this.authService.getToken();
        let headers = new HttpHeaders({
            'Content-Type': 'application/json'
        });

        if (token) {
            headers = headers.set('Authorization', `Bearer ${token}`);
        }
        return headers;
    }
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.serviceUrl}/products`, { headers: this.getHeaders()});
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`<span class="math-inline">\{this\.serviceUrl\}/products/</span>{id}`, { headers: this.getHeaders() });
  }
}