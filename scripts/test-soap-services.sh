#!/bin/bash

echo "🧪 Testando Serviços SOAP..."
echo "================================"

# Função para testar um serviço SOAP
test_soap_service() {
    local service_name=$1
    local url=$2
    local operation=$3
    local envelope=$4
    
    echo "📡 Testando $service_name - $operation"
    echo "URL: $url"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$url" \
        -H "Content-Type: text/xml; charset=utf-8" \
        -H "SOAPAction: \"urn:soa-ecommerce:v1:$operation\"" \
        -d "$envelope")
    
    http_code=$(echo "$response" | tail -n1)
    response_body=$(echo "$response" | head -n -1)
    
    if [ "$http_code" -eq 200 ]; then
        echo "✅ Sucesso (HTTP $http_code)"
        echo "Resposta: $response_body" | head -c 200
        echo "..."
    else
        echo "❌ Falha (HTTP $http_code)"
        echo "Resposta: $response_body"
    fi
    echo ""
}

# Teste 1: CreateCustomer
create_customer_envelope='<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <AuthHeader xmlns="urn:soa-ecommerce:v1:auth">
      <ApiKey>dev</ApiKey>
    </AuthHeader>
    <CorrelationId xmlns="urn:soa-ecommerce:v1:headers">12345</CorrelationId>
  </soap:Header>
  <soap:Body>
    <CreateCustomer xmlns="urn:soa-ecommerce:v1:customers">
      <Name>Teste Cliente</Name>
      <Email>teste@email.com</Email>
    </CreateCustomer>
  </soap:Body>
</soap:Envelope>'

test_soap_service "CustomerService" "http://localhost:7001/soap" "customers/ICustomerService/CreateCustomer" "$create_customer_envelope"

# Teste 2: CreateProduct
create_product_envelope='<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <AuthHeader xmlns="urn:soa-ecommerce:v1:auth">
      <ApiKey>dev</ApiKey>
    </AuthHeader>
    <CorrelationId xmlns="urn:soa-ecommerce:v1:headers">12346</CorrelationId>
  </soap:Header>
  <soap:Body>
    <CreateProduct xmlns="urn:soa-ecommerce:v1:catalog">
      <Name>Produto Teste</Name>
      <Price>99.99</Price>
      <Stock>50</Stock>
    </CreateProduct>
  </soap:Body>
</soap:Envelope>'

test_soap_service "CatalogService" "http://localhost:7002/soap" "catalog/ICatalogService/CreateProduct" "$create_product_envelope"

# Teste 3: PlaceOrder (Sucesso)
place_order_envelope='<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <AuthHeader xmlns="urn:soa-ecommerce:v1:auth">
      <ApiKey>dev</ApiKey>
    </AuthHeader>
    <CorrelationId xmlns="urn:soa-ecommerce:v1:headers">12347</CorrelationId>
  </soap:Header>
  <soap:Body>
    <PlaceOrder xmlns="urn:soa-ecommerce:v1:process">
      <CustomerId>11111111-1111-1111-1111-111111111111</CustomerId>
      <Items>
        <Item>
          <ProductId>33333333-3333-3333-3333-333333333333</ProductId>
          <Quantity>1</Quantity>
        </Item>
      </Items>
    </PlaceOrder>
  </soap:Body>
</soap:Envelope>'

test_soap_service "CompositionService" "http://localhost:7000/soap" "process/ICompositionService/PlaceOrder" "$place_order_envelope"

echo "🎉 Testes concluídos!"
