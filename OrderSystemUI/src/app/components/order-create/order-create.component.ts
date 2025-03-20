import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { ProductService } from '../../services/product.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { Customer } from '../../models/customer';
import { Product } from '../../models/product';
import { CustomerService } from '../../services/customer.service';

@Component({
  selector: 'app-order-create',
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.css'],
  standalone: false,
})
export class OrderCreateComponent implements OnInit {
  customers: Customer[] = [];
  products: Product[] = [];
  selectedCustomerId: number | null = null;
  selectedProductId: number | null = null;
  quantity: number = 1;
  createError: string | null = null;

  constructor(
    private orderService: OrderService,
    private productService: ProductService,
    private customerService: CustomerService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
      this.productService.getProducts().subscribe(products => this.products = products);
  }

  onSubmit(): void {
    this.createError = null;
    if (this.selectedCustomerId == null || this.selectedProductId == null || this.quantity <= 0) {
      this.showError('Please select a customer, a product, and enter a valid quantity.');
      return;
    }

    const orderData = {
      customerId: this.selectedCustomerId,
      productId: this.selectedProductId,
      quantity: this.quantity
    };

    this.orderService.createOrder(orderData).subscribe(
      newOrder => {
        console.log('Order created:', newOrder);
        this.router.navigate(['/orders']);
        this.snackBar.open('Order created successfully!', 'Close', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
          });
      },
      error => {
        console.error('Error creating order:', error);
        this.showError(error.error || 'Error creating order. Please try again.');

      }
    );
  }

  showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
}