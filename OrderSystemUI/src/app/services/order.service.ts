import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/order';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private serviceUrl = "http://localhost:5000/api";

  constructor(private http: HttpClient, private authService: AuthService) { }


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

  createOrder(order: any): Observable<Order> {
    return this.http.post<Order>(`${this.serviceUrl}/orders`, order, { headers: this.getHeaders() });
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.serviceUrl}/orders`, { headers: this.getHeaders() });
  }

  getOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`/orders/${id}`, { headers: this.getHeaders() });
  }

  updateOrder(order: Order): Observable<any> {
    return this.http.put(`${this.serviceUrl}/orders/${order.id}`, order, { headers: this.getHeaders() });
  }

    deleteOrder(id: number): Observable<any> {
        return this.http.delete(`${this.serviceUrl}/orders/${id}`, { headers: this.getHeaders() });
    }
}