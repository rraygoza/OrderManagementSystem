import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  standalone: false
})
export class ConfirmDialogComponent {
  message!: string;
  buttonText = {
    ok: 'Confirm',
    cancel: 'Cancel'
  };
  @Inject(MAT_DIALOG_DATA) private data: any;

  constructor(
    private dialogRef: MatDialogRef<ConfirmDialogComponent>) {
    if (this.data) {
      this.message = this.data.message || this.message;
      if (this.data.buttonText) {
        this.buttonText = this.data.buttonText;
      }
    }
  }

  onConfirmClick(): void {
    this.dialogRef.close(true);
  }
}