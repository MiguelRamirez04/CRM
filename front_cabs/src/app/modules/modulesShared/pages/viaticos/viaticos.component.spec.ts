import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViaticosComponent } from './viaticos.component';

describe('ViaticosComponent', () => {
  let component: ViaticosComponent;
  let fixture: ComponentFixture<ViaticosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViaticosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViaticosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
