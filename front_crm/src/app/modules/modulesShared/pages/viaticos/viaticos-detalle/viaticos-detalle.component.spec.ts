import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { ViaticosDetalleComponent } from './viaticos-detalle.component';

describe('ViaticosDetalleComponent', () => {
  let component: ViaticosDetalleComponent;
  let fixture: ComponentFixture<ViaticosDetalleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [ViaticosDetalleComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViaticosDetalleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
