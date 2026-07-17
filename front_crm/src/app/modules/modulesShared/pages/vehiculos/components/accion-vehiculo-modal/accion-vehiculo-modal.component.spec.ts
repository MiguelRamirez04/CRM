import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { AccionVehiculoModalComponent } from './accion-vehiculo-modal.component';

describe('AccionVehiculoModalComponent', () => {
  let component: AccionVehiculoModalComponent;
  let fixture: ComponentFixture<AccionVehiculoModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [AccionVehiculoModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccionVehiculoModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
