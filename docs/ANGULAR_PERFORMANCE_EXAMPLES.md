# Angular Performance Examples

This document provides practical examples for implementing performance optimizations in the Angular frontend.

## Table of Contents

1. [Lazy Loading](#lazy-loading)
2. [OnPush Change Detection](#onpush-change-detection)
3. [Virtual Scrolling](#virtual-scrolling)
4. [Image Lazy Loading](#image-lazy-loading)
5. [Input Debouncing](#input-debouncing)
6. [TrackBy Functions](#trackby-functions)
7. [Preloading Strategies](#preloading-strategies)
8. [Service Workers](#service-workers)

## Lazy Loading

### Route-Level Lazy Loading

**File**: `app.routes.ts`

```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.DashboardComponent)
  },
  {
    path: 'orders',
    loadChildren: () => import('./features/orders/orders.routes')
      .then(m => m.ORDERS_ROUTES)
  },
  {
    path: 'slips',
    loadChildren: () => import('./features/slips/slips.routes')
      .then(m => m.SLIP_ROUTES)
  }
];
```

**Benefits**: Reduces initial bundle size by 40-60%

### Component-Level Lazy Loading

**File**: `slip-list.component.ts`

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-slip-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slip-list">
      @if (showDetails()) {
        <app-slip-details [slip]="selectedSlip()" />
      }
    </div>
  `
})
export class SlipListComponent {
  showDetails = signal(false);
  selectedSlip = signal<Slip | null>(null);
  
  // Lazy load detail component only when needed
  async loadDetails(slip: Slip) {
    const { SlipDetailsComponent } = await import('./slip-details.component');
    this.selectedSlip.set(slip);
    this.showDetails.set(true);
  }
}
```

## OnPush Change Detection

### Basic OnPush Component

**File**: `order-card.component.ts`

```typescript
import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-order-card',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="order-card" (click)="onClick()">
      <h3>{{ order().orderNumber }}</h3>
      <p>Amount: {{ order().amount | currency }}</p>
      <span class="status" [class]="order().status">
        {{ order().status }}
      </span>
    </div>
  `,
  styles: [`
    .order-card {
      padding: 1rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      cursor: pointer;
    }
  `]
})
export class OrderCardComponent {
  order = input.required<Order>();
  orderClick = output<Order>();
  
  onClick() {
    this.orderClick.emit(this.order());
  }
}
```

**Benefits**: 50-70% reduction in change detection cycles

### OnPush with Signals

**File**: `order-list.component.ts`

```typescript
import { Component, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../services/order.service';
import { OrderCardComponent } from './order-card.component';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, OrderCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="order-list">
      <div class="filters">
        <button (click)="filterStatus.set('all')">All</button>
        <button (click)="filterStatus.set('pending')">Pending</button>
        <button (click)="filterStatus.set('paid')">Paid</button>
      </div>
      
      <div class="order-grid">
        @for (order of filteredOrders(); track order.id) {
          <app-order-card 
            [order]="order"
            (orderClick)="onOrderClick($event)" />
        }
      </div>
      
      <p>Showing {{ filteredOrders().length }} of {{ orders().length }} orders</p>
    </div>
  `
})
export class OrderListComponent {
  private orderService = inject(OrderService);
  
  orders = signal<Order[]>([]);
  filterStatus = signal<string>('all');
  
  // Computed signal for filtered orders
  filteredOrders = computed(() => {
    const status = this.filterStatus();
    if (status === 'all') {
      return this.orders();
    }
    return this.orders().filter(o => o.status === status);
  });
  
  ngOnInit() {
    this.loadOrders();
  }
  
  async loadOrders() {
    const orders = await this.orderService.getOrders();
    this.orders.set(orders);
  }
  
  onOrderClick(order: Order) {
    console.log('Order clicked:', order);
  }
}
```

## Virtual Scrolling

### Basic Virtual Scrolling

**File**: `transaction-list.component.ts`

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { TransactionService } from '../../services/transaction.service';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule, ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport 
      itemSize="60" 
      class="transaction-viewport">
      <div *cdkVirtualFor="let transaction of transactions(); trackBy: trackById"
           class="transaction-item">
        <div class="transaction-info">
          <span class="date">{{ transaction.date | date }}</span>
          <span class="description">{{ transaction.description }}</span>
        </div>
        <span class="amount" [class.positive]="transaction.amount > 0">
          {{ transaction.amount | currency }}
        </span>
      </div>
    </cdk-virtual-scroll-viewport>
  `,
  styles: [`
    .transaction-viewport {
      height: 600px;
      width: 100%;
      border: 1px solid #ddd;
    }
    
    .transaction-item {
      height: 60px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0 1rem;
      border-bottom: 1px solid #eee;
    }
  `]
})
export class TransactionListComponent {
  private transactionService = inject(TransactionService);
  
  transactions = signal<Transaction[]>([]);
  
  ngOnInit() {
    this.loadTransactions();
  }
  
  async loadTransactions() {
    const transactions = await this.transactionService.getAll();
    this.transactions.set(transactions);
  }
  
  trackById(index: number, item: Transaction): string {
    return item.id;
  }
}
```

**Benefits**: Renders only visible items, handles 10,000+ items smoothly

### Virtual Scrolling with Infinite Scroll

**File**: `infinite-scroll-list.component.ts`

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule, CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { ViewChild } from '@angular/core';

@Component({
  selector: 'app-infinite-scroll-list',
  standalone: true,
  imports: [CommonModule, ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport 
      itemSize="80" 
      class="viewport"
      (scrolledIndexChange)="onScroll($event)">
      <div *cdkVirtualFor="let order of orders(); trackBy: trackById"
           class="order-item">
        <h4>{{ order.orderNumber }}</h4>
        <p>{{ order.description }}</p>
      </div>
      
      @if (loading()) {
        <div class="loading">Loading more...</div>
      }
    </cdk-virtual-scroll-viewport>
  `
})
export class InfiniteScrollListComponent {
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;
  
  orders = signal<Order[]>([]);
  loading = signal(false);
  page = signal(1);
  hasMore = signal(true);
  
  ngOnInit() {
    this.loadMore();
  }
  
  async onScroll(index: number) {
    const end = this.viewport.getRenderedRange().end;
    const total = this.viewport.getDataLength();
    
    // Load more when scrolled to 80% of content
    if (end === total && !this.loading() && this.hasMore()) {
      await this.loadMore();
    }
  }
  
  async loadMore() {
    if (this.loading()) return;
    
    this.loading.set(true);
    
    const newOrders = await this.orderService.getOrders(
      this.page(),
      20
    );
    
    if (newOrders.length < 20) {
      this.hasMore.set(false);
    }
    
    this.orders.update(orders => [...orders, ...newOrders]);
    this.page.update(p => p + 1);
    this.loading.set(false);
  }
  
  trackById(index: number, item: Order): string {
    return item.id;
  }
}
```

## Image Lazy Loading

### Native Lazy Loading

**File**: `slip-image.component.ts`

```typescript
import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-slip-image',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slip-image-container">
      <img 
        [src]="imageUrl()" 
        [alt]="alt()"
        loading="lazy"
        class="slip-image"
        (error)="onError()"
        (load)="onLoad()" />
      
      @if (isLoading()) {
        <div class="skeleton-loader"></div>
      }
    </div>
  `,
  styles: [`
    .slip-image-container {
      position: relative;
      width: 100%;
      aspect-ratio: 3/4;
    }
    
    .slip-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 8px;
    }
    
    .skeleton-loader {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: loading 1.5s infinite;
      border-radius: 8px;
    }
    
    @keyframes loading {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `]
})
export class SlipImageComponent {
  imageUrl = input.required<string>();
  alt = input<string>('Slip image');
  
  isLoading = signal(true);
  hasError = signal(false);
  
  onLoad() {
    this.isLoading.set(false);
  }
  
  onError() {
    this.isLoading.set(false);
    this.hasError.set(true);
  }
}
```

### Progressive Image Loading

**File**: `progressive-image.component.ts`

```typescript
import { Component, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-progressive-image',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="image-wrapper">
      <!-- Low-quality placeholder -->
      <img 
        [src]="thumbnailUrl()"
        class="thumbnail"
        [class.loaded]="!isLoading()" />
      
      <!-- High-quality image -->
      <img 
        [src]="fullUrl()"
        class="full-image"
        [class.loaded]="!isLoading()"
        loading="lazy"
        (load)="onLoad()" />
    </div>
  `,
  styles: [`
    .image-wrapper {
      position: relative;
      overflow: hidden;
    }
    
    .thumbnail, .full-image {
      width: 100%;
      height: auto;
      transition: opacity 0.3s ease;
    }
    
    .thumbnail {
      position: absolute;
      top: 0;
      left: 0;
      filter: blur(10px);
      opacity: 1;
    }
    
    .thumbnail.loaded {
      opacity: 0;
    }
    
    .full-image {
      opacity: 0;
    }
    
    .full-image.loaded {
      opacity: 1;
    }
  `]
})
export class ProgressiveImageComponent {
  thumbnailUrl = input.required<string>();
  fullUrl = input.required<string>();
  
  isLoading = signal(true);
  
  onLoad() {
    this.isLoading.set(false);
  }
}
```

## Input Debouncing

### Search with Debouncing

**File**: `search-box.component.ts`

```typescript
import { Component, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="search-box">
      <input 
        type="text"
        [formControl]="searchControl"
        placeholder="Search orders..."
        class="search-input" />
      
      @if (isSearching()) {
        <span class="spinner">🔄</span>
      }
      
      @if (results().length > 0) {
        <div class="results">
          @for (result of results(); track result.id) {
            <div class="result-item" (click)="onResultClick(result)">
              {{ result.orderNumber }} - {{ result.description }}
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .search-box {
      position: relative;
      width: 100%;
    }
    
    .search-input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }
    
    .spinner {
      position: absolute;
      right: 1rem;
      top: 50%;
      transform: translateY(-50%);
    }
    
    .results {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: white;
      border: 1px solid #ddd;
      border-top: none;
      max-height: 300px;
      overflow-y: auto;
      z-index: 1000;
    }
    
    .result-item {
      padding: 0.75rem;
      cursor: pointer;
      border-bottom: 1px solid #eee;
    }
    
    .result-item:hover {
      background: #f5f5f5;
    }
  `]
})
export class SearchBoxComponent {
  private searchService = inject(SearchService);
  
  searchControl = new FormControl('');
  results = signal<Order[]>([]);
  isSearching = signal(false);
  
  resultSelected = output<Order>();
  
  ngOnInit() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300), // Wait 300ms after user stops typing
        distinctUntilChanged(), // Only emit when value actually changed
        tap(() => this.isSearching.set(true)),
        switchMap(term => this.searchService.search(term || ''))
      )
      .subscribe(results => {
        this.results.set(results);
        this.isSearching.set(false);
      });
  }
  
  onResultClick(result: Order) {
    this.resultSelected.emit(result);
    this.results.set([]);
    this.searchControl.setValue('', { emitEvent: false });
  }
}
```

**Benefits**: Reduces API calls by 80-90%

## TrackBy Functions

### Efficient List Rendering

**File**: `order-list.component.ts`

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="order-list">
      @for (order of orders(); track trackByOrderId($index, order)) {
        <div class="order-item">
          <h3>{{ order.orderNumber }}</h3>
          <p>{{ order.amount | currency }}</p>
        </div>
      }
    </div>
  `
})
export class OrderListComponent {
  orders = signal<Order[]>([]);
  
  // TrackBy function for better performance
  trackByOrderId(index: number, order: Order): string {
    return order.id;
  }
  
  // Alternative: track by multiple properties
  trackByOrderDetails(index: number, order: Order): string {
    return `${order.id}-${order.updatedAt}`;
  }
}
```

## Preloading Strategies

### Custom Preloading Strategy

**File**: `custom-preload-strategy.ts`

```typescript
import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of, timer } from 'rxjs';
import { mergeMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class CustomPreloadStrategy implements PreloadingStrategy {
  preload(route: Route, load: () => Observable<any>): Observable<any> {
    // Check if route should be preloaded
    if (route.data && route.data['preload']) {
      const delay = route.data['preloadDelay'] || 0;
      
      console.log(`Preloading ${route.path} after ${delay}ms`);
      
      return timer(delay).pipe(
        mergeMap(() => load())
      );
    }
    
    return of(null);
  }
}
```

**Usage in routes**:

```typescript
export const routes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component'),
    data: { preload: true, preloadDelay: 2000 }
  },
  {
    path: 'orders',
    loadChildren: () => import('./orders/orders.routes'),
    data: { preload: true, preloadDelay: 5000 }
  }
];
```

**Register in app config**:

```typescript
import { provideRouter, withPreloading } from '@angular/router';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withPreloading(CustomPreloadStrategy))
  ]
};
```

## Performance Checklist

- [ ] All routes use lazy loading
- [ ] Components use OnPush change detection
- [ ] Long lists use virtual scrolling
- [ ] Images use native lazy loading
- [ ] Search inputs use debouncing
- [ ] Lists use trackBy functions
- [ ] Custom preloading strategy configured
- [ ] Service worker enabled for caching
- [ ] Bundle size is optimized
- [ ] Production build uses AOT

## Measuring Performance

```typescript
// Use Performance API
const start = performance.now();
await this.loadData();
const end = performance.now();
console.log(`Load time: ${end - start}ms`);

// Use Chrome DevTools Performance tab
// 1. Open DevTools
// 2. Go to Performance tab
// 3. Click Record
// 4. Interact with your app
// 5. Stop recording
// 6. Analyze results
```

## References

- [Angular Performance Guide](https://angular.io/guide/performance-best-practices)
- [CDK Virtual Scrolling](https://material.angular.io/cdk/scrolling/overview)
- [Angular Signals](https://angular.io/guide/signals)
- [RxJS Operators](https://rxjs.dev/guide/operators)
