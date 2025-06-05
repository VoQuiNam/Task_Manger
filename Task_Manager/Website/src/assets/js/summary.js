import Chart from 'chart.js/auto';

export default {
  mounted() {
    const ctx = document.getElementById('statusChart').getContext('2d');
    new Chart(ctx, {
      type: 'pie',
      data: {
        labels: ['Completed', 'Updated', 'Created', 'Due Soon'],
        datasets: [{
          label: 'Work Items',
          data: [1, 5, 5, 1],
          backgroundColor: [
            '#198754',
            '#0d6efd',
            '#6f42c1',
            '#ffc107'
          ],
          borderWidth: 1
        }]
      },
      options: {
        plugins: {
          legend: {
            position: 'bottom'
          }
        }
      }
    });
  }
};
