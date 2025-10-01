import { Pipe, PipeTransform } from '@angular/core';
import { VerificationStatus, OrderStatus, TransactionStatus } from '../../core/models/domain.models';

@Pipe({
  name: 'statusColor',
  standalone: true
})
export class StatusColorPipe implements PipeTransform {
  transform(status: VerificationStatus | OrderStatus | TransactionStatus): string {
    const statusMap: { [key: string]: string } = {
      // Verification Status
      'Pending': 'bg-yellow-100 text-yellow-800',
      'Processing': 'bg-blue-100 text-blue-800',
      'Verified': 'bg-green-100 text-green-800',
      'Failed': 'bg-red-100 text-red-800',
      'Rejected': 'bg-red-100 text-red-800',
      'ManualReview': 'bg-purple-100 text-purple-800',
      
      // Order Status
      'PendingPayment': 'bg-yellow-100 text-yellow-800',
      'Paid': 'bg-green-100 text-green-800',
      'Completed': 'bg-green-100 text-green-800',
      'Cancelled': 'bg-gray-100 text-gray-800',
      'Refunded': 'bg-orange-100 text-orange-800',
      
      // Transaction Status
      'Success': 'bg-green-100 text-green-800',
    };
    
    return statusMap[status] || 'bg-gray-100 text-gray-800';
  }
}
