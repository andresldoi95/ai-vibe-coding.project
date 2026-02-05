import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import router from './router'
import App from './App.vue'

// PrimeVue CSS - Load BEFORE custom styles
import 'primevue/resources/themes/lara-light-blue/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'

// Custom styles - Load AFTER PrimeVue
import './assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  locale: {
    startsWith: 'Empieza con',
    contains: 'Contiene',
    notContains: 'No contiene',
    endsWith: 'Termina con',
    equals: 'Igual a',
    notEquals: 'Diferente de',
    noFilter: 'Sin filtro',
    filter: 'Filtro',
    lt: 'Menor que',
    lte: 'Menor o igual que',
    gt: 'Mayor que',
    gte: 'Mayor o igual que',
    dateIs: 'Fecha es',
    dateIsNot: 'Fecha no es',
    dateBefore: 'Fecha antes de',
    dateAfter: 'Fecha después de',
    clear: 'Limpiar',
    apply: 'Aplicar',
    matchAll: 'Coincidir todo',
    matchAny: 'Coincidir cualquiera',
    addRule: 'Agregar regla',
    removeRule: 'Eliminar regla',
    accept: 'Sí',
    reject: 'No',
    choose: 'Elegir',
    upload: 'Subir',
    cancel: 'Cancelar',
    completed: 'Completado',
    pending: 'Pendiente',
    dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
    dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'],
    dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa'],
    monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
    monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
    chooseYear: 'Elegir año',
    chooseMonth: 'Elegir mes',
    chooseDate: 'Elegir fecha',
    prevDecade: 'Década anterior',
    nextDecade: 'Década siguiente',
    prevYear: 'Año anterior',
    nextYear: 'Año siguiente',
    prevMonth: 'Mes anterior',
    nextMonth: 'Mes siguiente',
    prevHour: 'Hora anterior',
    nextHour: 'Hora siguiente',
    prevMinute: 'Minuto anterior',
    nextMinute: 'Minuto siguiente',
    prevSecond: 'Segundo anterior',
    nextSecond: 'Segundo siguiente',
    am: 'am',
    pm: 'pm',
    today: 'Hoy',
    weekHeader: 'Sem',
    firstDayOfWeek: 1,
    dateFormat: 'dd/mm/yy',
    weak: 'Débil',
    medium: 'Medio',
    strong: 'Fuerte',
    passwordPrompt: 'Ingrese una contraseña',
    emptyFilterMessage: 'Sin opciones disponibles',
    searchMessage: '{0} resultados disponibles',
    selectionMessage: '{0} elementos seleccionados',
    emptySelectionMessage: 'No hay elementos seleccionados',
    emptySearchMessage: 'No se encontraron resultados',
    emptyMessage: 'No hay datos disponibles',
    aria: {
      trueLabel: 'Verdadero',
      falseLabel: 'Falso',
      nullLabel: 'No seleccionado',
      star: '1 estrella',
      stars: '{star} estrellas',
      selectAll: 'Todos los elementos seleccionados',
      unselectAll: 'Todos los elementos deseleccionados',
      close: 'Cerrar',
      previous: 'Anterior',
      next: 'Siguiente',
      navigation: 'Navegación',
      scrollTop: 'Desplazar arriba',
      moveTop: 'Mover arriba',
      moveUp: 'Mover hacia arriba',
      moveDown: 'Mover hacia abajo',
      moveBottom: 'Mover abajo',
      moveToTarget: 'Mover al objetivo',
      moveToSource: 'Mover al origen',
      moveAllToTarget: 'Mover todo al objetivo',
      moveAllToSource: 'Mover todo al origen',
      pageLabel: 'Página {page}',
      firstPageLabel: 'Primera página',
      lastPageLabel: 'Última página',
      nextPageLabel: 'Siguiente página',
      prevPageLabel: 'Página anterior',
      rowsPerPageLabel: 'Filas por página',
      previousPageLabel: 'Página anterior',
      jumpToPageDropdownLabel: 'Ir al menú de página',
      jumpToPageInputLabel: 'Ir a la entrada de página',
      selectRow: 'Fila seleccionada',
      unselectRow: 'Fila deseleccionada',
      expandRow: 'Fila expandida',
      collapseRow: 'Fila colapsada',
      showFilterMenu: 'Mostrar menú de filtros',
      hideFilterMenu: 'Ocultar menú de filtros',
      filterOperator: 'Operador de filtro',
      filterConstraint: 'Restricción de filtro',
      editRow: 'Editar fila',
      saveEdit: 'Guardar edición',
      cancelEdit: 'Cancelar edición',
      listView: 'Vista de lista',
      gridView: 'Vista de cuadrícula',
      slide: 'Deslizar',
      slideNumber: '{slideNumber}',
      zoomImage: 'Ampliar imagen',
      zoomIn: 'Acercar',
      zoomOut: 'Alejar',
      rotateRight: 'Rotar a la derecha',
      rotateLeft: 'Rotar a la izquierda'
    }
  }
})
app.use(ToastService)
app.use(ConfirmationService)

app.mount('#app')
