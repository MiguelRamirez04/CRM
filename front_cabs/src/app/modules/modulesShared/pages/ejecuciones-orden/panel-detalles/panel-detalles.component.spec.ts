import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelDetallesComponent } from './panel-detalles.component';

describe('PanelDetallesComponent', () => {
  let component: PanelDetallesComponent;
  let fixture: ComponentFixture<PanelDetallesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PanelDetallesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PanelDetallesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
