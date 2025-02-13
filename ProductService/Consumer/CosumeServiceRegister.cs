using ProductService.Consumers;
using ProductService.Services;

namespace ProductService.Consumer
{
       
    public class CosumeServiceRegister : BackgroundService
    {
        private readonly IOrderConsumer _productConsumer;
        private readonly IEmailConsumer _emailConsumer;
        public CosumeServiceRegister(IOrderConsumer productConsumer, IEmailConsumer emailConsumer)
        {
            _emailConsumer = emailConsumer;
            _productConsumer = productConsumer;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _emailConsumer.ConsumeMessageAsync();
                await _productConsumer.UpdateProductQuantity();
                
            }
        }
    }
}
