import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css'],
  standalone: false,
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  displayedColumns: string[] = ['id', 'customerId', 'productId', 'quantity', 'totalAmount', 'orderDate', 'actions'];
  isLoading = true;
  constructor(private orderService: OrderService, public dialog: MatDialog) { }

  ngOnInit(): void {
   this.loadOrders();
  }

  loadOrders(): void{
    this.isLoading = true;
    this.orderService.getOrders().subscribe(
      orders => {
        this.orders = orders;
        this.isLoading = false;
      },
      error => {
        console.error(error);
        this.isLoading = false;
      }
    );
  }
 deleteOrder(id: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        message: 'Are you sure you want to delete this order?',
        buttonText: {
          ok: 'Delete',
          cancel: 'Cancel'
        }
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
         this.orderService.deleteOrder(id).subscribe(
            () => {
                this.loadOrders();
            },
            error => console.error("Error deleting order:", error)
        );
      }
    });
  }
}