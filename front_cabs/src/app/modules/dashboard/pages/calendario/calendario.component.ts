import { Component, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FullCalendarModule, FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import esLocale from '@fullcalendar/core/locales/es';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';

// Plugins
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';
import interactionPlugin from '@fullcalendar/interaction';
import momentPlugin from '@fullcalendar/moment';
import rrulePlugin from '@fullcalendar/rrule';
import multiMonthPlugin from '@fullcalendar/multimonth';
import adaptivePlugin from '@fullcalendar/adaptive';

@Component({
  selector: 'app-calendario',
  standalone: true,
  imports: [CommonModule, FullCalendarModule,UiHeaderComponent],
  templateUrl: './calendario.component.html',
  styleUrls: ['./calendario.component.css']
})
export class CalendarioComponent implements AfterViewInit {
  @ViewChild('calendarPrincipal') calendarComponent!: FullCalendarComponent;
  @ViewChild('calendarSecundario') calendarSecundarioComponent!: FullCalendarComponent;

  calendarTitle: string = '';
  calendarTitleSecundario: string = '';
  activeView: string = 'multiMonthYear';

  calendarOptions: CalendarOptions = {
    plugins: [
      dayGridPlugin,
      timeGridPlugin,
      listPlugin,
      interactionPlugin,
      momentPlugin,
      rrulePlugin,
      multiMonthPlugin,
      adaptivePlugin
    ],
    initialView: this.activeView,
    headerToolbar: false,
    locale: esLocale,
    buttonText: {
      today: 'Hoy',
      year: 'Año',
      month: 'Mes',
      week: 'Semana',
      day: 'Día',
      timeGridDay: 'Día',
      dayGridDay: 'Día',
    },
    events: [
      { title: 'Evento simple', date: '2025-10-22' },
      {
        title: 'Evento recurrente',
        rrule: {
          freq: 'weekly',
          interval: 1,
          byweekday: ['mo', 'we', 'fr'],
          dtstart: '2025-10-01T10:30:00',
          until: '2025-12-31'
        }
      }
    ],
    editable: true,
    selectable: true,
    nowIndicator: true,
    aspectRatio: 1.5,
    datesSet: this.onDatesSet.bind(this)
  };

  calendarOptionsSecundario: CalendarOptions = {
    plugins: [
      dayGridPlugin,
      timeGridPlugin,
      interactionPlugin,
      momentPlugin,
      rrulePlugin,
      multiMonthPlugin,
      adaptivePlugin
    ],
    initialView: 'dayGridMonth',
    headerToolbar: false,
    locale: esLocale,
    buttonText: {
      today: 'Hoy',
      month: 'Mes',
      week: 'Semana',
      day: 'Día'
    },
    events: [
      { title: 'Reunión interna', date: '2025-10-25' },
      { title: 'Entrega de proyecto', date: '2025-10-28' }
    ],
    editable: false,
    selectable: true,
    nowIndicator: true,
    aspectRatio: 1.5,
    datesSet: this.onDatesSetSecundario.bind(this)
  };

  ngAfterViewInit(): void {
    this.calendarTitle = this.calendarComponent.getApi().view.title;
    this.calendarTitleSecundario = this.calendarSecundarioComponent.getApi().view.title;
  }

  onDatesSet() {
    this.calendarTitle = this.calendarComponent.getApi().view.title;
  }

  onDatesSetSecundario() {
    this.calendarTitleSecundario = this.calendarSecundarioComponent.getApi().view.title;
  }

  setView(view: string) {
    this.activeView = view;
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.changeView(view);
    this.calendarTitle = calendarApi.view.title;
  }

  getTitle(): string {
    switch (this.activeView) {
      case 'multiMonthYear':
        return 'Vista anual';
      case 'dayGridMonth':
        return 'Vista mensual';
      case 'timeGridWeek':
        return 'Vista semanal';
      case 'timeGridDay':
        return 'Vista diaria';
      default:
        return 'Calendario';
    }
  }

  goToPrev() {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.prev();
    this.calendarTitle = calendarApi.view.title;
  }

  goToNext() {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.next();
    this.calendarTitle = calendarApi.view.title;
  }

  goToToday() {
    const calendarApi = this.calendarComponent.getApi();
    calendarApi.today();
    this.calendarTitle = calendarApi.view.title;
  }

  goToPrevSecundario() {
    const calendarApi = this.calendarSecundarioComponent.getApi();
    calendarApi.prev();
    this.calendarTitleSecundario = calendarApi.view.title;
  }

  goToNextSecundario() {
    const calendarApi = this.calendarSecundarioComponent.getApi();
    calendarApi.next();
    this.calendarTitleSecundario = calendarApi.view.title;
  }

  goToTodaySecundario() {
    const calendarApi = this.calendarSecundarioComponent.getApi();
    calendarApi.today();
    this.calendarTitleSecundario = calendarApi.view.title;
  }
}
