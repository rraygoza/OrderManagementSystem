export interface Order {
    id: number;
    customerId: number;
    productId: number;
    quantity: number;
    totalAmount: number;
    orderDate: Date;
  }