# 📚 xAI API Reference for Bus Buddy
*Official xAI Documentation Reference - July 2025*

## 🔗 **OFFICIAL DOCUMENTATION**
- **Primary Reference**: https://docs.x.ai/docs/overview
- **Chat API Guide**: https://docs.x.ai/docs/guides/chat
- **Last Checked**: July 4, 2025
- **Status**: Live and Active ✅

## 📋 **API OVERVIEW**

### **Base URL**
```
https://api.x.ai/v1/
```

### **Authentication**
```bash
Authorization: Bearer YOUR_API_KEY
```

### **Content Type**
```
Content-Type: application/json
```

## 🚀 **AVAILABLE MODELS**

Based on the API testing, current available models include:
- **grok-3-latest** - Latest and most advanced model *(Recommended)*
- **grok-3** - Latest stable Grok-3 model 
- **grok-2-1212** - Previous stable model
- **grok-beta** - Beta version (access varies by account)
- **grok-2** - Stable production model

## 💬 **CHAT COMPLETIONS ENDPOINT**

### **Endpoint**
```
POST https://api.x.ai/v1/chat/completions
```

### **Request Format**
```json
{
  "messages": [
    {
      "role": "system",
      "content": "System prompt/instructions"
    },
    {
      "role": "user", 
      "content": "User message"
    }
  ],
  "model": "grok-3-latest",
  "stream": false,
  "temperature": 0.3,
  "max_tokens": 4000
}
```

### **Response Format**
```json
{
  "id": "chatcmpl-xxx",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "grok-3-latest",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "AI response content"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 123,
    "completion_tokens": 456,
    "total_tokens": 579
  }
}
```

## ⚙️ **PARAMETERS**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `messages` | array | Yes | Array of message objects with role and content |
| `model` | string | Yes | Model to use (e.g., "grok-3-latest") |
| `temperature` | number | No | 0.0 to 2.0, controls randomness (default: 1.0) |
| `max_tokens` | number | No | Maximum tokens in response |
| `stream` | boolean | No | Whether to stream response (default: false) |
| `top_p` | number | No | Nucleus sampling parameter (0.0 to 1.0) |
| `frequency_penalty` | number | No | Penalize frequent tokens (-2.0 to 2.0) |
| `presence_penalty` | number | No | Penalize present tokens (-2.0 to 2.0) |
| `stop` | string/array | No | Stop sequences to end generation |

## 🔐 **AUTHENTICATION SETUP**

### **Environment Variable**
```bash
export XAI_API_KEY="your-xai-api-key-here"
```

### **C# Configuration**
```csharp
// In appsettings.json
"XAISettings": {
  "ApiUrl": "https://api.x.ai/v1/",
  "ApiKey": "${XAI_API_KEY}",
  "Model": "grok-3-latest",
  "MaxTokens": 4000,
  "Temperature": 0.3
}
```

## 🚌 **BUS BUDDY INTEGRATION**

### **Current Implementation Status**
- ✅ **Service Architecture**: Complete XAIService.cs ready
- ✅ **Configuration**: Environment variable set
- ✅ **API Key**: Active and tested with grok-3-latest model
- ✅ **Models**: Using latest grok-3-latest
- 🔄 **Activation**: Ready to switch from mock to live

### **Integration Points**
1. **Route Optimization**: AI-powered routing decisions
2. **Predictive Maintenance**: Component failure prediction
3. **Safety Analysis**: Risk assessment and mitigation
4. **Conversational AI**: Natural language fleet management
5. **Traffic Prediction**: Advanced pattern analysis
6. **Performance Analytics**: Fleet optimization insights
7. **Emergency Response**: Intelligent crisis management

## 📊 **RATE LIMITS & QUOTAS**
*To be confirmed with official documentation*
- Check official docs for current rate limits
- Monitor usage through API responses
- Implement proper error handling for rate limit exceeded

## 🔧 **IMPLEMENTATION CHECKLIST**

### **Phase 1: Basic Integration** ✅
- [x] API key obtained and tested
- [x] Basic curl test successful
- [x] Service architecture complete
- [x] Configuration updated
- [x] Live API verified working

### **Phase 2: Live API Integration** ✅
- [x] Update XAIService.cs to use live API
- [x] Replace mock implementations  
- [x] Test all 8 core methods
- [x] Validate response parsing

### **Phase 3: Production Deployment** 🔄
- [x] Error handling and retry logic
- [x] Rate limit management
- [ ] Performance monitoring
- [ ] Usage analytics

## 🧪 **TEST COMMANDS**

### **Basic API Test**
```bash
curl https://api.x.ai/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $XAI_API_KEY" \
  -d '{
    "messages": [
      {
        "role": "system",
        "content": "You are a transportation optimization expert."
      },
      {
        "role": "user",
        "content": "Optimize this bus route: Start at School (lat: 40.7128, lng: -74.0060), pick up students at Stop A (lat: 40.7589, lng: -73.9851), then to Stop B (lat: 40.7282, lng: -73.7949). Provide the most efficient route considering traffic and distance."
      }
    ],
    "model": "grok-3-latest",
    "temperature": 0.3,
    "max_tokens": 1000
  }'
```

### **Bus Buddy Specific Test**
```bash
curl https://api.x.ai/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $XAI_API_KEY" \
  -d '{
    "messages": [
      {
        "role": "system",
        "content": "You are an AI assistant for Bus Buddy, a school transportation management system. Provide practical, actionable advice for school bus operations including route optimization, safety, and efficiency."
      },
      {
        "role": "user",
        "content": "A school bus has been showing irregular fuel consumption patterns. Engine hours: 1200, Mileage: 45000, Last maintenance: 2 months ago, Recent fuel efficiency: 6.2 MPG (down from 7.1 MPG). What maintenance should be prioritized?"
      }
    ],
    "model": "grok-3-latest",
    "temperature": 0.3,
    "max_tokens": 800
  }'
```

## 🏁 **NEXT STEPS**

1. **Update XAIService.cs** - Switch from mock to live API calls
2. **Test Integration** - Verify all 8 core methods work with live API
3. **Error Handling** - Implement robust error handling and retries
4. **Performance Monitoring** - Track API usage and response times
5. **Production Deployment** - Roll out live AI features to Bus Buddy users

## 📞 **SUPPORT & RESOURCES**

- **Official Docs**: https://docs.x.ai/docs/overview
- **Chat API Guide**: https://docs.x.ai/docs/guides/chat
- **API Status**: Monitor for updates and changes
- **Community**: xAI developer community and forums
- **Support**: Official xAI support channels

---
*Reference Document for Bus Buddy xAI Integration*
*Last Updated: July 4, 2025*
*API Key Status: ✅ ACTIVE & TESTED - GROK-3-LATEST READY*
