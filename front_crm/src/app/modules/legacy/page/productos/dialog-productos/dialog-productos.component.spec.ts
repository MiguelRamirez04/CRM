import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { DialogProductosComponent } from './dialog-productos.component';

describe('DialogProductosComponent', () => {
  let component: DialogProductosComponent;
  let fixture: ComponentFixture<DialogProductosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [DialogProductosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DialogProductosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
