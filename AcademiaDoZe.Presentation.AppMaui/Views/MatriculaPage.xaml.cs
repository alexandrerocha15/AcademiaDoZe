using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            CalcularDataFim();
            CarregarRestricoes();
        }
    }

    private void OnPlanoChanged(object? sender, EventArgs e)
    {
        CalcularDataFim();
    }

    private void OnInicioChanged(object? sender, DateChangedEventArgs e)
    {
        CalcularDataFim();
    }

    private void CalcularDataFim()
    {
        if (BindingContext is not MatriculaViewModel vm)
            return;

        var inicio = vm.Matricula.DataInicio;

        if (inicio == default)
            return;

        var fim = vm.Matricula.Plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };

        vm.Matricula.DataFim = fim;
        FimDatePicker.Date = fim.ToDateTime(TimeOnly.MinValue);
    }

    private bool _carregandoRestricoes;

    private void CarregarRestricoes()
    {
        if (BindingContext is not MatriculaViewModel vm)
            return;

        _carregandoRestricoes = true;

        DiabetesCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.Diabetes);

        PressaoAltaCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.PressaoAlta);

        LabirintiteCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.Labirintite);

        AlergiasCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.Alergias);

        RespiratoriosCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.ProblemasRespiratorios);

        RemedioCheck.IsChecked =
            vm.RestricaoSelecionada(AppMatriculaRestricoes.RemedioContinuo);

        _carregandoRestricoes = false;
    }

    private void OnRestricaoChanged(
        object? sender, CheckedChangedEventArgs e)
    {
        if (_carregandoRestricoes ||
            BindingContext is not MatriculaViewModel vm)
            return;

        AppMatriculaRestricoes restricao;

        if (sender == DiabetesCheck)
            restricao = AppMatriculaRestricoes.Diabetes;
        else if (sender == PressaoAltaCheck)
            restricao = AppMatriculaRestricoes.PressaoAlta;
        else if (sender == LabirintiteCheck)
            restricao = AppMatriculaRestricoes.Labirintite;
        else if (sender == AlergiasCheck)
            restricao = AppMatriculaRestricoes.Alergias;
        else if (sender == RespiratoriosCheck)
            restricao = AppMatriculaRestricoes.ProblemasRespiratorios;
        else if (sender == RemedioCheck)
            restricao = AppMatriculaRestricoes.RemedioContinuo;
        else
            return;

        vm.AtualizarRestricao(restricao, e.Value);
    }
}
