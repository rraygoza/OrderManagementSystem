import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
selector: 'app-product-list',
templateUrl: './product-list.component.html',
styleUrls: ['./product-list.component.css'],
standalone: false,
})
export class ProductListComponent implements OnInit {

products: Product[] = [];
isLoading = true;

constructor(private productService: ProductService) { }

ngOnInit(): void {
  this.productService.getProducts().subscribe(
    products => {
      this.products = products;
      this.isLoading = false;
    },
    error => {
      console.error(error);
      this.isLoading = false;
    }
  );
}
}