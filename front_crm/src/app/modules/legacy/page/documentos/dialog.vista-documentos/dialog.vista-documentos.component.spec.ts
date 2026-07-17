import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { DialogVistaDocumentosComponent } from './dialog.vista-documentos.component';

describe('DialogVistaDocumentosComponent', () => {
  let component: DialogVistaDocumentosComponent;
  let fixture: ComponentFixture<DialogVistaDocumentosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClientTesting()],
imports: [DialogVistaDocumentosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DialogVistaDocumentosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
