# Калькулятор расчетной стоимость объекта
Калькулятор расчетной стоимость объекта создан для расчета стоимости объекта на основе **Revit модели** и баз данных цен из OneDrive (на момент написания readme файла)

---
## Процесс запуска калькулятора на вашем ПК
Во первых данный калькулятор не встроен в **TeamPanel**, там на него лишь ссылка в созданной кноопке и если вы зайдете в проект тим панели и увидите там проект **MathCalcPrice** то там явно что-то не так, но в самом проекте **TeamPanel** есть класс `App.cs` в проекте [TeamPanel](https://gitlab.com/bim_development/teampanel/-/blob/master/TeamPanel/App.cs) там будет подобная конструкция:

```C#
AddButtonBitmap(
                "bim_apartment_info.Class1",
                "\nProfitbase",
                folder + "\\bim-apartment-info.dll",
                "bim_apartment_info.Class1",
                ribbonPanelBim,
                " Excel       ",
                Properties.Resources.Profitbase_8080);

```
---
В этой конструкции и указываються все данные для вашей кнопки в **TeamPanel'и**, в коде панели будет описан данный класс `AddButtonBitmap` с коментариями и вы поймете что да как после сборки проета необходимо поместить все необходимые dll сборки проектов в папку `\\obmen\АПГ\Базы и дополнения\Классификатор\Пакет Классификатора ред. 1.10.2018\КП ГК ПРОГРЕСС\Работа над календарным планом\Для Саши\teampanel` там будет находиться файл **`teampanel_install.ps1`** он и запускает ~~установку~~ копирование файлов в папку аддонов для ревита  `C:\ProgramData\Autodesk\Revit\Addins\2019` 

В общем если так и не стало понятно просто поройтесь в коде и папки с аддонами для ревита и вы легко поймете как все устроенно.

После проведения всех необходимых манипуляций с проектом и тип панелью вы получите свою кнопку в RibonPanel'и можете начинать работать с ней.

---
## Процесс запуска калькулятора на других компах (например в АПГ или у экономистов)
Проводите ровно теже манипуляции только сидя за столом того, кому это надо.

# Логика работы калькулятора
## Коротко о процессе разработки и почему все так плохо

Тут и начинаються циганские фокусы, ведь этот проект пилили не один нормальный человек а аж два недопрограммиста с абсолютно разными подходами, поэтому дела обстоят так **Саша** пилил половну логики расчета самого РСО, а я (**Никита**) уже допиливал проект за ним, исправляя всячекие косяки и вводя все необходимые изменения.

Также советую привыкнуть что тут будет много всяческого мусора, оставшегося после правок в коде, я их старался максимально почистить, но там столько говна по типу ~~малоли пригодится~~ было что все не вычистить.

## Логика работа
Тут все очень просто. В проекте есть класс `EntryPoint.cs` как понятно из названия он и является точкой входа в проект в данном классе описан проект запуска `UI` приложения для работы с РСО.

После запуска `UI` необходимо выбрать какой объект нам необходимо заполнить в таблице.
>Процесс выборки столбцов для приложения происходит по такой схеме: берутся три столбца из [calc_template (2)](https://oooprogressit.sharepoint.com/:x:/s/msteams_559812/EX5PufQPLU1DpvfSY0Cf4BMB_-jZU5ifC4LqY9opeN7Spg?e=R7a5Lw) со столбцами помечеными как *Текущий объект* и названием объекта. Кароче посмотри в документ там все будет понятно сразу. Все это действо проходит в классе `MainWindowViewModel.cs`

После чего мы поподаем в класс `MainWindow.xaml.cs` (ебал я рот тех, кто пишет десктоп без хотя бы базового понимания [MVVM](https://www.youtube.com/watch?v=uMNYu0p3MP4&t=9956s) паттерна и делает всю логику в самой форме) в котором есть функция `CreateExcelRSOAsync` (пройдите полностью по ней и впринципе поймете что да как), в ней и происходит вся магия кратко описанная ниже

>метод `LoadDataFromOneDrive`: загружает на локальный ПК документы: 
```C#
private async Task LoadDataFromOneDrive()
        {
            string pathPrices, pathGroups;

            OneDriveController oneDriveController = new OneDriveController();

            pathPrices = await OneDriveController.DowloandExcelFile(Cache.SelectedCostViewElement, "database_prices_progress.xlsx");
            pathGroups = await OneDriveController.DowloandExcelFile(Cache.SelectedJobViewElement, "spisok-grupp.xlsx");

            var pathCalcDb = Paths.CalcDbExcelPath;
            _db = Cache.Ensure(pathPrices, pathGroups, pathCalcDb, Paths.MainDir);
            //isdone = true;
        }
```
>>[calc_template (2)](https://oooprogressit.sharepoint.com/:x:/s/msteams_559812/EX5PufQPLU1DpvfSY0Cf4BMB_-jZU5ifC4LqY9opeN7Spg?e=R7a5Lw): это шаблон по которому строится РСО, он же и является выходным продуктом плагина, только заполненный
>
>>[01.01.02.01 database_prices_progress](https://oooprogressit.sharepoint.com/:x:/s/BIM/EeFN4nn2EPZFh85qjiCeBjgBJ8pifIswitD-p0d9GBtzGg?e=BpyD9i): это аналог нормальной базы данных, только в экселе со всеми необходимыми данными для расчетами
>
>>[01.01.02.05 Список групп работ](https://oooprogressit.sharepoint.com/:x:/s/BIM/EUwaMe-DkytFtk-WbBEGQ3ABM-GWx94dwe-jU_Pq6I20Fg?e=RsySNa): файл со списком групп работ
>
>>Еще есть один секретный файл, его необходимо копировать после первого запуска плагина до нажатия кнопки расчета, когда плагин создаст все необходимые папки и файлы. В папку `Documents\RSO` необходимо поместить файл `bd_calc.xlsx` (я не знаю что это за файл, за что он отвечает и откуда его еще брать кроме как копирования это файла с другого компа руками, мб это когда нибудь исправит кто-то)

>метод `GetElemsForRSO`: получает элементы для создания РСО из ревит модели, а также проводит фильтрацию элментов ищя подходящие элменты для расчета
```C#
public (List<AnnexElement> AnnexResult, List<(ElementTemp e, string docName)> NonValid) GetElemsForRSO()
        {
            List<List<AnnexElement>> rawAnnex = new List<List<AnnexElement>>();
            List<List<(ElementTemp element, string docName)>> rawNonValid = new List<List<(ElementTemp element, string docName)>>();
            Annex annex = new Annex(_db);
            var ext = new ElementGetter(_settings.ClassificatorsCache);
            int count = 0;
            int countOfDocs = _settings.LinkedFiles.Where(x => x.IsChecked).Count();
            Parallel.ForEach(_settings.LinkedFiles, new ParallelOptions { MaxDegreeOfParallelism = countOfDocs > Environment.ProcessorCount ? Environment.ProcessorCount : countOfDocs },
                (doc) =>
                {
                    if (!doc.IsChecked) return;

                    var raw = ext.Work(doc.Doc, _db);
                    var result = annex.ElementsHandling(raw.elems.Where(x => x.Valid).ToList(), _settings);
                    //sl.Lock(); 
                    count++;
                    rawAnnex.Add(result.result);
                    rawNonValid.Add(raw.elems.Where(x => !x.Valid).Select(x => (x, doc.Name)).ToList());
                    rawNonValid.Add(result.nonValid.Select(x => (x, doc.Name)).ToList());
                    //sl.Unlock();
                }
            );
            return (rawAnnex.SelectMany(x => x).ToList(), rawNonValid.SelectMany(x => x).ToList());
        }
```

>метод `CalculatorTemplate.Create`: расчет и создание РСО

>метод `CalculatorTemplate.Save`: просто сохранение документа

**После чего выдается `MessageBox` с адрессом сохраненнго РСО**