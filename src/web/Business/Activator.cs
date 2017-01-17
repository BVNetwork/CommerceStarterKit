/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;

namespace OxxCommerceStarterKit.Web.Business
{
    /// <summary>
    /// Creates an instance of <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Activator<T>
    {
        private IObjectInstanceCache _objectInstanceCache;
        private delegate T ObjectActivator(params object[] args);

        private IObjectInstanceCache ObjectInstanceCache
        {
            get
            {
                return _objectInstanceCache ?? (_objectInstanceCache = ServiceLocator.Current.GetInstance<IObjectInstanceCache>());
            }
            set
            {
                _objectInstanceCache = value;
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="T"/>.
        /// </summary>
        /// <param name="args">Arguments, used for constructor creation, and for creating a generic type</param>
        /// <returns>The requested <see cref="T"/>.</returns>
        /// <remarks>The supports generic arguments.</remarks>
        public T Activate(params object[] args)
        {
            return Activate(typeof(T), args);
        }

        /// <summary>
        /// Creates an instance of the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to create.</param>
        /// <param name="constructorArguments">Arguments, used for constructor creation.</param>
        /// <returns>The requested <paramref name="type"/>.</returns>
        /// <remarks>The supports generic arguments.</remarks>
        public T Activate(Type type, params object[] constructorArguments)
        {
            var argumentTypes = constructorArguments
                .Select(x => x != null ? x.GetOriginalType() : typeof(object))
                .ToArray();

            var cacheKey = String.Concat(type.GetHashCode(), "#", String.Join(":", argumentTypes.Select(x => x.GetHashCode())));
            var objectActivator = ObjectInstanceCache.ReadThrough<ObjectActivator>(cacheKey, () =>
            {
                var finalType = type.IsGenericTypeDefinition ? type.MakeGenericType(argumentTypes) : type;
                return GetObjectActivator(finalType, argumentTypes);
            });
            return objectActivator(constructorArguments);
        }

        /// <summary>
        /// Gets the objector activator delegate, that will be used to create the requested object.
        /// </summary>
        /// <param name="type">The object type to create an activator for.</param>
        /// <param name="constructorArgumentTypes">The argument types for the <paramref name="type"/> constructor.</param>
        /// <returns>The delegate with instructions to create the specified type.</returns>
        private static ObjectActivator GetObjectActivator(Type type, Type[] constructorArgumentTypes)
        {
            // If type is an open generic type, then create a closed type with generic arguments defined.
            // Type closedType = type.IsGenericTypeDefinition ? type.MakeGenericType(constructorArgumentTypes) : type;

            ConstructorInfo constructorInfo = type.GetConstructor(constructorArgumentTypes);
            var delegateParameterExpression = Expression.Parameter(typeof(object[]), "args");

            var typeExpressions = CreateTypeExpressions(constructorArgumentTypes, delegateParameterExpression);
            return CreateDelegate(constructorInfo, typeExpressions, delegateParameterExpression);
        }

        /// <summary>
        /// Creates expressions for the constructor arguments.
        /// </summary>
        /// <param name="constructorArgumentTypes">The constructor argument types.</param>
        /// <param name="delegateParameterExpression">The expression for the delegate parameter.</param>
        /// <returns>Expressions for the constructor arguments.</returns>
        private static IEnumerable<Expression> CreateTypeExpressions(Type[] constructorArgumentTypes, Expression delegateParameterExpression)
        {
            var expressionResult = new Expression[constructorArgumentTypes.Length];
            for (var i = 0; i < constructorArgumentTypes.Length; i++)
            {
                var paramType = constructorArgumentTypes[i];

                var indexExpression = Expression.Constant(i);
                var paramAccessorExpression = Expression.ArrayIndex(delegateParameterExpression, indexExpression);
                var paramCastExpression = Expression.Convert(paramAccessorExpression, paramType);

                expressionResult[i] = paramCastExpression;
            }
            return expressionResult;
        }

        /// <summary>
        /// Creates <see cref="ObjectActivator"/>, that will be used to create the requested type.
        /// </summary>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <param name="constructorTypeExpressions">Expressions for the constructor types.</param>
        /// <param name="delegateParameterExpression">The delegate parameter expression.</param>
        /// <returns>The <see cref="ObjectActivator"/></returns>
        private static ObjectActivator CreateDelegate(ConstructorInfo constructorInfo, IEnumerable<Expression> constructorTypeExpressions, ParameterExpression delegateParameterExpression)
        {
            var constructorExpression = Expression.New(constructorInfo, constructorTypeExpressions);
            var lambdaExpression = Expression.Lambda(typeof(ObjectActivator), constructorExpression, delegateParameterExpression);

            return (ObjectActivator)lambdaExpression.Compile();
        }
    }
}
