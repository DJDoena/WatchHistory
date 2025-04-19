using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;
using Icon = DoenaSoft.AbstractionLayer.UIServices.Icon;

namespace DoenaSoft.WatchHistory.ShowReport.Implementations;

internal sealed class ShowReportViewModel : IShowReportViewModel
{
    private readonly IDataManager _dataManager;

    private readonly IIOServices _ioServices;

    private readonly IUIServices _uiServices;

    private readonly IClipboardServices _clipboardServices;

    private readonly string _userName;

    private DateTime _date;

    public ShowReportViewModel(IDataManager dataManager, IIOServices ioServices, IUIServices uiServices, IClipboardServices clipboardServices, string userName)
    {
        _dataManager = dataManager;
        _ioServices = ioServices;
        _uiServices = uiServices;
        _clipboardServices = clipboardServices;
        _userName = userName;

        this.ReportDayCommand = new RelayCommand(this.ReportDay);
        this.ReportMonthCommand = new RelayCommand(this.ReportMonth);
        this.CancelCommand = new RelayCommand(this.Cancel);

        _date = DateTime.Now.Date;
    }

    #region IShowReportViewModel

    public ICommand ReportDayCommand { get; }

    public ICommand ReportMonthCommand { get; }

    public ICommand CancelCommand { get; }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value;

                this.RaisePropertyChanged(nameof(this.Date));
            }
        }
    }

    public event EventHandler<CloseEventArgs> Closing;

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private void ReportDay()
    {
        var entries = this.GetFilteredEntries(new DayCalculationProcessor(_dataManager, _userName, this.Date));

        var success = this.CopyReportToClipboard(new DayTextProcessor(_ioServices, this.Date, entries, _userName));

        if (success)
        {
            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }
    }

    private void ReportMonth()
    {
        var entries = this.GetFilteredEntries(new MonthCalculationProcessor(_dataManager, _userName, this.Date));

        var success = this.CopyReportToClipboard(new MonthTextProcessor(this.Date, entries, _userName));

        if (success)
        {
            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }
    }

    private void Cancel() => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

    private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

    private IEnumerable<FileEntry> GetFilteredEntries(CalculationProcessorBase calculationProcessor)
    {
        var entries = calculationProcessor.GetEntries();

        if (entries.Any(fileEntry => !fileEntry.VideoLengthSpecified))
        {
            _uiServices.ShowMessageBox("At least one entry does not have a running time specified. Calculated watch time will be incorrect.", "Incorrect Watch Time", Buttons.OK, Icon.Warning);
        }

        return entries;
    }

    private bool CopyReportToClipboard(TextProcessorBase textProcessor)
    {
        var text = textProcessor.GetText();

        var success = _clipboardServices.SetText(text);

        if (success)
        {
            _uiServices.ShowMessageBox("Report successfully copied to clipboard.", "Success", Buttons.OK, Icon.Information);
        }
        else
        {
            _uiServices.ShowMessageBox("Report could not be copied to clipboard. Please try again", "Clipboard Error", Buttons.OK, Icon.Warning);
        }

        return success;
    }
}