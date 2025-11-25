import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViaticosDialogComponent } from './viaticos-dialog.component';

describe('ViaticosDialogComponent', () => {
  let component: ViaticosDialogComponent;
  let fixture: ComponentFixture<ViaticosDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViaticosDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViaticosDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
