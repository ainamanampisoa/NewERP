<!-- @{
    var dataGrossPay = new decimal[12];
    var dataNetPay = new decimal[12];
    var dataDeduction = new decimal[12];
    var labels = new string[12];

    foreach (var mois in ViewBag.TotauxAnnuels.TotauxMensuels)
    {
        int index = mois.Mois - 1; 
        if (index >= 0 && index < 12)
        {
            labels[index] = mois.NomMois;
            dataGrossPay[index] = mois.TotalGrossPay;
            dataNetPay[index] = mois.TotalNetPay;
            dataDeduction[index] = mois.TotalDeduction;
        }
    }
} -->




<!-- <script>
    const ctx = document.getElementById('salaireChart').getContext('2d');

    const labels = @Html.Raw(Json.Serialize(labels));
    const dataGrossPay = @Html.Raw(Json.Serialize(dataGrossPay));
    const dataNetPay = @Html.Raw(Json.Serialize(dataNetPay));
    const dataDeduction = @Html.Raw(Json.Serialize(dataDeduction));

    const salaireChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Salaire Brut',
                    data: dataGrossPay,
                    borderColor: 'rgb(7, 63, 100)',
                    // backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    pointBackgroundColor: 'rgba(54, 162, 235, 1)',
                    pointRadius: 4,
                    fill: true,
                    tension: 0.3
                },
                {
                    label: 'Salaire Net',
                    data: dataNetPay,
                    borderColor: 'rgb(22, 209, 209)',
                    // backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    pointBackgroundColor: 'rgba(75, 192, 192, 1)',
                    pointRadius: 4,
                    fill: true,
                    tension: 0.3
                },
                {
                    label: 'Déductions',
                    data: dataDeduction,
                    borderColor: 'rgb(146, 26, 52)',
                    // backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                    pointRadius: 4,
                    fill: true,
                    tension: 0.3
                }
            ]
        },
        options: {
            responsive: true,
            interaction: {
                mode: 'index',
                intersect: false
            },
            stacked: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Évolution des salaires par année',
                    font: {
                        size: 18,
                        weight: 'bold'
                    },
                    padding: {
                        top: 10,
                        bottom: 30
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': ' + context.formattedValue.replace(/\B(?=(\d{3})+(?!\d))/g, " ") + ' Ar';
                        }
                    }
                },
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 500000,
                        callback: function(value) {
                            return value.toLocaleString('fr-FR') + ' $';
                        }
                    },
                    title: {
                        display: true,
                        text: 'Montant ($)',
                        font: {
                            weight: 'bold'
                        }
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Mois',
                        font: {
                            weight: 'bold'
                        }
                    }
                }
            }
        }
    });
</script> -->